using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Requests.Appointment;
using api_barber.src.Interfaces;
using api_barber.src.Requests;
using api_barber.src.Utils;
using api_barber.Models.Enums;
using MongoDB.Bson;
using System.Linq;

namespace api_barber.Services
{
    public class AppointmentService(
        IAppointmentRepository repository,
        IScheduleRepository scheduleRepository,
        IServiceService serviceService) : IAppointmentService
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(string barbershopId, string? customerId = null, string? barberId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barbershopId))
                {
                    return new(new List<dynamic>(), 200, "Agendamentos listados com sucesso");
                }

                var matchDoc = new BsonDocument
                {
                    {"deleted", new BsonDocument("$ne", true)},
                    {"barbershop_id", barbershopId.Trim()}
                };

                if (!string.IsNullOrWhiteSpace(customerId))
                {
                    matchDoc.Add("customer_id", customerId.Trim());
                }

                if (!string.IsNullOrWhiteSpace(barberId))
                {
                    matchDoc.Add("barber_id", barberId.Trim());
                }

                List<BsonDocument> pipeline =
                [
                    new("$match", matchDoc),
                    new("$addFields", new BsonDocument
                    {
                        {"customerObjectId", new BsonDocument("$toObjectId", "$customer_id")},
                        {"barberObjectId", new BsonDocument("$toObjectId", "$barber_id")},
                        {"serviceTypeObjectId", new BsonDocument("$toObjectId", "$service_type_id")}
                    }),
                    new("$lookup", new BsonDocument
                    {
                        {"from", "users"},
                        {"localField", "customerObjectId"},
                        {"foreignField", "_id"},
                        {"as", "customers"}
                    }),
                    new("$lookup", new BsonDocument
                    {
                        {"from", "users"},
                        {"localField", "barberObjectId"},
                        {"foreignField", "_id"},
                        {"as", "barbers"}
                    }),
                    new("$lookup", new BsonDocument
                    {
                        {"from", "services_types"},
                        {"localField", "serviceTypeObjectId"},
                        {"foreignField", "_id"},
                        {"as", "serviceTypes"}
                    }),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"date", 1},
                        {"hour", 1},

                        {"customerName", new BsonDocument("$first", "$customers.name")},
                        {"barberName", new BsonDocument("$first", "$barbers.name")},
                        {"serviceTypeName", new BsonDocument("$first", "$serviceTypes.name")},

                        {"serviceId", new BsonDocument("$ifNull", new BsonArray { "$service_id", "$serviceId", "" })},
                        {"serviceTypeId", new BsonDocument("$ifNull", new BsonArray { "$service_type_id", "$serviceTypeId", "" })},
                        {"barberId", new BsonDocument("$ifNull", new BsonArray { "$barber_id", "$barberId", "" })},
                        {"customerId", new BsonDocument("$ifNull", new BsonArray { "$customer_id", "$customerId", "" })},
                        {"barbershopId", new BsonDocument("$ifNull", new BsonArray { "$barbershop_id", "$barbershopId", "" })},
                        {"value", new BsonDocument("$toDouble", "$value")},
                        {"status", 1},
                        {"notes", 1},
                        {"cancelNotes", new BsonDocument("$ifNull", new BsonArray { "$cancel_notes", "$cancelNotes", "" })},
                        {"paymentStatus", new BsonDocument("$ifNull", new BsonArray { "$payment_status", "$paymentStatus", "" })},
                        {"createdAt", 1}
                    }),
                    new("$sort", new BsonDocument { { "date", 1 }, { "hour", 1 } } )
                ];

                List<dynamic> list = await repository.GetAllAsync(pipeline);

                return new(list, 200, "Agendamentos listados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }

        public async Task<ResponseApi<Appointment>> GetByIdAsync(string id)
        {
            try
            {
                Appointment entity = await repository.GetByIdAsync(id);
                if (entity is null) return new(null, 404, "Agendamento não encontrado");

                return new(entity, 200, "Agendamento buscado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }

        public async Task<ResponseApi<List<string>>> GetAvailableSlotsAsync(string barberId, DateTime date, string barbershopId, string? serviceId = null, string? customerId = null)
        {
            try
            {
                var schedulesResponse = await scheduleRepository.GetAllEntitiesAsync(barbershopId);
                int targetDay = ((int)date.DayOfWeek + 6) % 7;
                var schedule = schedulesResponse?.FirstOrDefault(s => s.BarberId == barberId && s.Day == targetDay && s.Active);

                if (schedule == null) return new(new List<string>(), 200, "Nenhuma escala para este dia.");

                int serviceDurationMinutes = schedule.IntervalMinutes > 0 ? schedule.IntervalMinutes : 30;
                if (!string.IsNullOrWhiteSpace(serviceId))
                {
                    var srvRes = await serviceService.GetByIdAsync(serviceId);
                    if (srvRes.Data != null && srvRes.Data.DurationMinutes.HasValue && srvRes.Data.DurationMinutes.Value > 0)
                    {
                        serviceDurationMinutes = srvRes.Data.DurationMinutes.Value;
                    }
                }

                var existingAppointments = await repository.GetByBarberAndDateAsync(barberId, date, barbershopId);
                var validAppointments = existingAppointments.Where(a => a.Status != AppointmentStatusEnum.Cancelado && !a.Deleted).ToList();

                List<Appointment> validCustomerAppointments = [];
                if (!string.IsNullOrWhiteSpace(customerId))
                {
                    var custApts = await repository.GetByCustomerAndDateAsync(customerId, date, barbershopId);
                    validCustomerAppointments = custApts.Where(a => a.Status != AppointmentStatusEnum.Cancelado && !a.Deleted).ToList();
                }

                var availableSlots = new List<string>();
                var currentTime = schedule.StartHour;
                int step = schedule.IntervalMinutes > 0 ? schedule.IntervalMinutes : 30;
                TimeSpan serviceDuration = TimeSpan.FromMinutes(serviceDurationMinutes);

                while (currentTime + serviceDuration <= schedule.EndHour)
                {
                    var slotEnd = currentTime + serviceDuration;
                    bool isSlotValid = true;

                    if (schedule.BreakStart.HasValue && schedule.BreakEnd.HasValue && schedule.BreakEnd > schedule.BreakStart)
                    {
                        if (currentTime < schedule.BreakEnd.Value && slotEnd > schedule.BreakStart.Value)
                        {
                            isSlotValid = false;
                        }
                    }

                    if (isSlotValid)
                    {
                        foreach (var apt in validAppointments)
                        {
                            if (TimeSpan.TryParse(apt.Hour, out var aptStart))
                            {
                                var aptDuration = TimeSpan.FromMinutes(30);
                                if (!string.IsNullOrWhiteSpace(apt.ServiceId))
                                {
                                    var srv = await serviceService.GetByIdAsync(apt.ServiceId);
                                    if (srv.Data != null && srv.Data.DurationMinutes.HasValue && srv.Data.DurationMinutes.Value > 0)
                                    {
                                        aptDuration = TimeSpan.FromMinutes(srv.Data.DurationMinutes.Value);
                                    }
                                }
                                var aptEnd = aptStart + aptDuration;
                                if (currentTime < aptEnd && slotEnd > aptStart)
                                {
                                    isSlotValid = false;
                                    break;
                                }
                            }
                            else if (apt.Hour == currentTime.ToString(@"hh\:mm"))
                            {
                                isSlotValid = false;
                                break;
                            }
                        }
                    }

                    if (isSlotValid && validCustomerAppointments.Count > 0)
                    {
                        foreach (var custApt in validCustomerAppointments)
                        {
                            if (TimeSpan.TryParse(custApt.Hour, out var custAptStart))
                            {
                                var custAptDuration = TimeSpan.FromMinutes(30);
                                if (!string.IsNullOrWhiteSpace(custApt.ServiceId))
                                {
                                    var srv = await serviceService.GetByIdAsync(custApt.ServiceId);
                                    if (srv.Data != null && srv.Data.DurationMinutes.HasValue && srv.Data.DurationMinutes.Value > 0)
                                    {
                                        custAptDuration = TimeSpan.FromMinutes(srv.Data.DurationMinutes.Value);
                                    }
                                }
                                var custAptEnd = custAptStart + custAptDuration;
                                if (currentTime < custAptEnd && slotEnd > custAptStart)
                                {
                                    isSlotValid = false;
                                    break;
                                }
                            }
                            else if (custApt.Hour == currentTime.ToString(@"hh\:mm"))
                            {
                                isSlotValid = false;
                                break;
                            }
                        }
                    }

                    if (isSlotValid)
                    {
                        availableSlots.Add(currentTime.ToString(@"hh\:mm"));
                    }

                    currentTime = currentTime.Add(TimeSpan.FromMinutes(step));
                }

                return new(availableSlots, 200, "Slots obtidos com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<Appointment>> CreateAsync(CreateAppointmentRequest request)
        {
            try
            {
                Appointment entity = ObjectMapper.Map<CreateAppointmentRequest, Appointment>(request);
                if (entity.Status == 0) entity.Status = AppointmentStatusEnum.Marcado;
                Appointment created = await repository.CreateAsync(entity);
                if (created is null) return new(null, 400, "Falha ao criar agendamento");

                return new(created, 201, "Agendamento criado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<Appointment>> UpdateAsync(UpdateAppointmentRequest request)
        {
            try
            {
                Appointment existed = await repository.GetByIdAsync(request.Id);
                if (existed is null) return new(null, 404, "Agendamento não encontrado");

                Appointment entity = ObjectMapper.Map<UpdateAppointmentRequest, Appointment>(request);
                entity.CreatedAt = existed.CreatedAt;
                entity.CreatedBy = existed.CreatedBy;

                Appointment updated = await repository.UpdateAsync(entity);
                if (updated is null) return new(null, 400, "Falha ao atualizar agendamento");

                return new(updated, 200, "Agendamento atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }

        public async Task<ResponseApi<Appointment>> UpdateStatusAsync(UpdateAppointmentStatusRequest request)
        {
            try
            {
                Appointment existed = await repository.GetByIdAsync(request.Id);
                if (existed is null) return new(null, 404, "Agendamento não encontrado");

                existed.Status = request.Status;
                if (!string.IsNullOrEmpty(request.CancelNotes))
                {
                    existed.CancelNotes = request.CancelNotes;
                }
                existed.UpdatedBy = request.UpdatedBy;
                existed.UpdatedAt = DateTime.UtcNow;

                Appointment updated = await repository.UpdateAsync(existed);
                if (updated is null) return new(null, 400, "Falha ao atualizar status do agendamento");

                return new(updated, 200, "Status atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<Appointment>> DeleteAsync(DeleteRequest request)
        {
            try
            {
                Appointment existed = await repository.GetByIdAsync(request.Id);
                if (existed is null) return new(null, 404, "Agendamento não encontrado");

                existed.Deleted = true;
                existed.DeletedAt = DateTime.UtcNow;
                existed.DeletedBy = request.DeletedBy;

                Appointment deleted = await repository.DeleteAsync(existed);
                if (deleted is null) return new(null, 400, "Falha ao excluir agendamento");

                return new(deleted, 200, "Agendamento excluído com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion
    }
}
