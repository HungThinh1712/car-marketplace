"use client";

import { Button, Form, Input, Select, type FormInstance } from "antd";
import { useRouter } from "next/navigation";
import { useEffect, useRef } from "react";

export type FilterValues = {
  make: string;
  model: string;
  minYear: string;
  maxYear: string;
  minPrice: string;
  maxPrice: string;
  maxMileage: string;
  location: string;
  status: string;
};

export function ListingFilters({ values }: { values: FilterValues }) {
  const router = useRouter();
  const formRef = useRef<FormInstance<FilterValues>>(null);
  const applyRef = useRef<() => void>(() => {});
  const stayOnFilters = useRef(false);

  useEffect(() => {
    formRef.current?.setFieldsValue({ ...values, status: values.status || "available" });
    if (!stayOnFilters.current) {
      return;
    }

    stayOnFilters.current = false;
    document.getElementById("filters")?.scrollIntoView({ block: "start" });
  }, [values]);

  function showFilters(path: string) {
    stayOnFilters.current = true;
    const keepFiltersInView = () => {
      document.getElementById("filters")?.scrollIntoView({ block: "start" });
    };
    router.push(path, { scroll: false });
    window.setTimeout(keepFiltersInView, 0);
    window.setTimeout(keepFiltersInView, 100);
  }

  function onFinish(fields: FilterValues) {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(fields)) {
      const text = String(value ?? "").trim();
      if (!text || (key === "status" && text === "available")) {
        continue;
      }

      params.set(key, text);
    }

    const query = params.toString();
    showFilters(query ? `/?${query}` : "/");
  }

  function apply() {
    const read = (id: string) => (document.getElementById(id) as HTMLInputElement | null)?.value ?? "";
    const statusLabel = document.querySelector("#filters .ant-select-content")?.textContent?.trim();
    const status = statusLabel === "Sold" ? "sold" : statusLabel === "Available and sold" ? "all" : "available";
    onFinish({
      make: read("make"),
      model: read("model"),
      minYear: read("minYear"),
      maxYear: read("maxYear"),
      minPrice: read("minPrice"),
      maxPrice: read("maxPrice"),
      maxMileage: read("maxMileage"),
      location: read("location"),
      status,
    });
  }

  applyRef.current = apply;

  return (
    <Form
      id="filters"
      ref={formRef}
      className="filters"
      layout="vertical"
      initialValues={{ ...values, status: values.status || "available" }}
      onFinish={onFinish}
      onSubmitCapture={(event) => event.preventDefault()}
    >
      <Form.Item label="Make" name="make">
        <Input />
      </Form.Item>
      <Form.Item label="Model" name="model">
        <Input />
      </Form.Item>
      <Form.Item label="Year from" name="minYear">
        <Input inputMode="numeric" />
      </Form.Item>
      <Form.Item label="Year to" name="maxYear">
        <Input inputMode="numeric" />
      </Form.Item>
      <Form.Item label="Price from" name="minPrice">
        <Input inputMode="decimal" />
      </Form.Item>
      <Form.Item label="Price to" name="maxPrice">
        <Input inputMode="decimal" />
      </Form.Item>
      <Form.Item label="Maximum mileage (km)" name="maxMileage">
        <Input inputMode="numeric" />
      </Form.Item>
      <Form.Item label="Location" name="location">
        <Input />
      </Form.Item>
      <Form.Item label="Status" name="status">
        <Select
          options={[
            { value: "available", label: "Available" },
            { value: "sold", label: "Sold" },
            { value: "all", label: "Available and sold" },
          ]}
        />
      </Form.Item>
      <div className="actions">
        <Button type="primary" htmlType="button" onClick={() => applyRef.current()}>
          Apply filters
        </Button>
        <Button
          onClick={() => showFilters("/")}
        >
          Clear filters
        </Button>
      </div>
    </Form>
  );
}
