"use client";

import { Alert, Button, Form, Input } from "antd";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useState } from "react";
import { problemText, readProblem } from "@/lib/problem";

function SignInForm() {
  const router = useRouter();
  const search = useSearchParams();
  const [error, setError] = useState("");

  async function onFinish(values: { email: string; password: string }) {
    setError("");
    const response = await fetch("/api/v1/sessions", {
      method: "POST",
      headers: { "content-type": "application/json", accept: "application/json" },
      body: JSON.stringify(values),
    });

    if (!response.ok) {
      setError(problemText(await readProblem(response)));
      return;
    }

    const next = search.get("next");
    router.push(next && next.startsWith("/") ? next : "/sell/listings");
    router.refresh();
  }

  return (
    <Form className="form" layout="vertical" onFinish={onFinish}>
      <h1>Sign in</h1>
      {error ? <Alert type="error" message={error} /> : null}
      <Form.Item label="Email" name="email" rules={[{ required: true, type: "email" }]}>
        <Input type="email" />
      </Form.Item>
      <Form.Item label="Password" name="password" rules={[{ required: true }]}>
        <Input.Password />
      </Form.Item>
      <Button type="primary" htmlType="submit">
        Sign in
      </Button>
    </Form>
  );
}

export default function SignInPage() {
  return (
    <Suspense>
      <SignInForm />
    </Suspense>
  );
}
