"use client";

import { Alert, Button, Form, Input } from "antd";
import { useState } from "react";
import { problemText, readProblem } from "@/lib/problem";

export function InquiryForm({
  listingId,
  make,
  model,
  year,
}: {
  listingId: string;
  make: string;
  model: string;
  year: number;
}) {
  const [form] = Form.useForm();
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  async function onFinish(values: { name: string; email: string; phone?: string; message: string }) {
    setError("");
    setMessage("");
    const phone = values.phone?.trim();
    const response = await fetch(`/api/v1/listings/${listingId}/inquiries`, {
      method: "POST",
      headers: { "content-type": "application/json", accept: "application/json" },
      body: JSON.stringify({
        name: values.name,
        email: values.email,
        message: values.message,
        ...(phone ? { phone } : {}),
      }),
    });

    if (!response.ok) {
      setError(problemText(await readProblem(response)));
      return;
    }

    const created = (await response.json()) as { make: string; model: string; year: number };
    setMessage(`Your inquiry about the ${created.year} ${created.make} ${created.model} was sent.`);
    form.resetFields();
  }

  return (
    <Form className="form" form={form} layout="vertical" onFinish={onFinish}>
      <h2>Ask about this {year} {make} {model}</h2>
      {error ? <Alert type="error" message={error} /> : null}
      {message ? <Alert type="success" message={message} /> : null}
      <Form.Item label="Name" name="name" rules={[{ required: true, max: 80 }]}>
        <Input maxLength={80} />
      </Form.Item>
      <Form.Item label="Email" name="email" rules={[{ required: true, type: "email", max: 254 }]}>
        <Input type="email" maxLength={254} />
      </Form.Item>
      <Form.Item label="Phone (optional)" name="phone">
        <Input maxLength={32} />
      </Form.Item>
      <Form.Item label="Message" name="message" rules={[{ required: true, min: 10, max: 1000 }]}>
        <Input.TextArea rows={5} maxLength={1000} />
      </Form.Item>
      <Button type="primary" htmlType="submit">
        Send inquiry
      </Button>
    </Form>
  );
}
