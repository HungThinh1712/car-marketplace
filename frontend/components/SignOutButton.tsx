"use client";

import { Button } from "antd";
import { useRouter } from "next/navigation";

export function SignOutButton() {
  const router = useRouter();

  async function signOut() {
    await fetch("/api/v1/sessions/current", { method: "DELETE" });
    router.push("/");
    router.refresh();
  }

  return (
    <Button onClick={signOut}>Sign out</Button>
  );
}
