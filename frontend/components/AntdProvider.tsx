"use client";

import { AntdRegistry } from "@ant-design/nextjs-registry";
import { ConfigProvider } from "antd";

export function AntdProvider({ children }: { children: React.ReactNode }) {
  return (
    <AntdRegistry>
      <ConfigProvider
        theme={{
          token: {
            colorPrimary: "#d90429",
            colorPrimaryHover: "#fd052f",
            colorLink: "#d90429",
            colorText: "#020102",
            colorBgBase: "#ffffff",
            borderRadius: 0,
            fontFamily: "var(--font-body), Poppins, sans-serif",
          },
        }}
      >
        {children}
      </ConfigProvider>
    </AntdRegistry>
  );
}
