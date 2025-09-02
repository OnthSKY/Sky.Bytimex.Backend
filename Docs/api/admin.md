# Admin Module

### ProductController — GET /api/admin/products/v1
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List products

**Request**
- **Route params:** none
- **Query params:** ProductFilterRequest
- **Body:** `none`

**Response**
- **200 OK:** `ProductListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/products/v1"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { ProductListResponse } from "@/shared/models";

  const { data } = await clientFetch<ProductListResponse>("/api/admin/products/v1");
  ```

### ProductCategoryController — GET /api/admin/product-categories/v1
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List product categories

**Request**
- **Route params:** none
- **Query params:** none
- **Body:** `none`

**Response**
- **200 OK:** `ProductCategoryListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/product-categories/v1"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { ProductCategoryListResponse } from "@/shared/models";

  const { data } = await clientFetch<ProductCategoryListResponse>("/api/admin/product-categories/v1");
  ```

### BrandController — GET /api/admin/brands/v2
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List brands

**Request**
- **Route params:** none
- **Query params:** BrandFilterRequest
- **Body:** `none`

**Response**
- **200 OK:** `BrandListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/brands/v2"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { BrandListResponse } from "@/shared/models";

  const { data } = await clientFetch<BrandListResponse>("/api/admin/brands/v2");
  ```

### OrderController — GET /api/admin/orders/v2
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List orders

**Request**
- **Route params:** none
- **Query params:** OrderFilterRequest
- **Body:** `none`

**Response**
- **200 OK:** `OrderListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/orders/v2"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { OrderListResponse } from "@/shared/models";

  const { data } = await clientFetch<OrderListResponse>("/api/admin/orders/v2");
  ```

### PaymentController — GET /api/admin/payments/v1
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List payments

**Request**
- **Route params:** none
- **Query params:** PaymentFilterRequest
- **Body:** `none`

**Response**
- **200 OK:** `PaymentListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/payments/v1"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { PaymentListResponse } from "@/shared/models";

  const { data } = await clientFetch<PaymentListResponse>("/api/admin/payments/v1");
  ```

### ShipmentController — GET /api/admin/shipments/v1
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List shipments

**Request**
- **Route params:** none
- **Query params:** ShipmentFilterRequest
- **Body:** `none`

**Response**
- **200 OK:** `ShipmentListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/shipments/v1"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { ShipmentListResponse } from "@/shared/models";

  const { data } = await clientFetch<ShipmentListResponse>("/api/admin/shipments/v1");
  ```

### RoleController — GET /api/admin/roles/v1/all
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List roles

**Request**
- **Route params:** none
- **Query params:** RoleFilterRequest
- **Body:** `none`

**Response**
- **200 OK:** `RoleListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/roles/v1/all"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { RoleListResponse } from "@/shared/models";

  const { data } = await clientFetch<RoleListResponse>("/api/admin/roles/v1/all");
  ```

### PermissionController — GET /api/admin/permissions/v1/all
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List permissions

**Request**
- **Route params:** none
- **Query params:** PermissionFilterRequest
- **Body:** `none`

**Response**
- **200 OK:** `PermissionListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/permissions/v1/all"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { PermissionListResponse } from "@/shared/models";

  const { data } = await clientFetch<PermissionListResponse>("/api/admin/permissions/v1/all");
  ```

### ReturnController — GET /api/admin/returns/v1
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List returns

**Request**
- **Route params:** none
- **Query params:** ReturnFilterRequest
- **Body:** `none`

**Response**
- **200 OK:** `ReturnListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/returns/v1"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { ReturnListResponse } from "@/shared/models";

  const { data } = await clientFetch<ReturnListResponse>("/api/admin/returns/v1");
  ```

### InvoiceController — GET /api/admin/invoices/v1
**Module:** Admin  
**Auth:** Authorize  
**Summary:** List invoices

**Request**
- **Route params:** none
- **Query params:** InvoiceFilterRequest
- **Body:** `none`

**Response**
- **200 OK:** `InvoiceListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/admin/invoices/v1"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { InvoiceListResponse } from "@/shared/models";

  const { data } = await clientFetch<InvoiceListResponse>("/api/admin/invoices/v1");
  ```

### KycController — POST /api/admin/kyc/approve/v1
**Module:** Admin  
**Auth:** Authorize  
**Summary:** Approve KYC

**Request**
- **Route params:** none
- **Query params:** none
- **Body:** `KycApprovalRequest`

**Response**
- **200 OK:** `KycStatusResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X POST "$BASE_URL/api/admin/kyc/approve/v1" -H "Content-Type: application/json" -d '{}'
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { KycApprovalRequest, KycStatusResponse } from "@/shared/models";

  const body: KycApprovalRequest = {};
  const { data } = await clientFetch<KycStatusResponse>("/api/admin/kyc/approve/v1", { method: "POST", body });
  ```

