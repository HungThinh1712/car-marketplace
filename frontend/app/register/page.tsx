"use client";

import { Alert, Button, Form, Input } from "antd";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { problemText, readProblem } from "@/lib/problem";

export default function RegisterPage() {
  const router = useRouter();
  const [error, setError] = useState("");

  async function onFinish(values: { displayName: string; email: string; password: string }) {
    setError("");
    const response = await fetch("/api/v1/sellers", {
      method: "POST",
      headers: { "content-type": "application/json", accept: "application/json" },
      body: JSON.stringify(values),
    });

    if (!response.ok) {
      setError(problemText(await readProblem(response)));
      return;
    }

    router.push("/sell/listings");
    router.refresh();
  }

  return (
    <Form className="form" layout="vertical" onFinish={onFinish}>
      <h1>Create a seller account</h1>
      {error ? <Alert type="error" message={error} /> : null}
      <Form.Item label="Display name" name="displayName" rules={[{ required: true, max: 80 }]}>
        <Input maxLength={80} />
      </Form.Item>
      <Form.Item label="Email" name="email" rules={[{ required: true, type: "email", max: 254 }]}>
        <Input type="email" maxLength={254} />
      </Form.Item>
      <Form.Item label="Password" name="password" rules={[{ required: true, min: 8, max: 128 }]}>
        <Input.Password maxLength={128} />
      </Form.Item>
      <Button type="primary" htmlType="submit">
        Register
      </Button>
    </Form>
  );
}
