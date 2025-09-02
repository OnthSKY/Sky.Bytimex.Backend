# Vendor Module

### ProductController — POST /api/vendor/products/v1
**Module:** Vendor  
**Auth:** Authorize  
**Summary:** Create product

**Request**
- **Route params:** none
- **Query params:** none
- **Body:** `CreateProductRequest`

**Response**
- **200 OK:** `ProductDetailResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X POST "$BASE_URL/api/vendor/products/v1" -H "Content-Type: application/json" -d '{}'
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { CreateProductRequest, ProductDetailResponse } from "@/shared/models";

  const body: CreateProductRequest = {};
  const { data } = await clientFetch<ProductDetailResponse>("/api/vendor/products/v1", { method: "POST", body });
  ```

