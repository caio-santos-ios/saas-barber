"use client";

import type { AnchorHTMLAttributes, MouseEvent } from "react";

type EventParams = Record<string, string>;

type TrackedLinkProps = AnchorHTMLAttributes<HTMLAnchorElement> & {
  eventName?: string;
  eventParams?: EventParams;
};

declare global {
  interface Window {
    gtag?: (command: "event", eventName: string, params?: EventParams) => void;
  }
}

export default function TrackedLink({
  eventName,
  eventParams,
  onClick,
  children,
  ...props
}: TrackedLinkProps) {
  function handleClick(event: MouseEvent<HTMLAnchorElement>) {
    if (eventName && typeof window !== "undefined" && window.gtag) {
      window.gtag("event", eventName, eventParams);
    }
    onClick?.(event);
  }

  return (
    <a {...props} onClick={handleClick}>
      {children}
    </a>
  );
}
