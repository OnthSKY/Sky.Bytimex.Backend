# Storefront Module

### ProductController — GET /api/user/products/v1
**Module:** Storefront  
**Auth:** AllowAnonymous  
**Summary:** List products for storefront

**Request**
- **Route params:** none
- **Query params:** none
- **Body:** `none`

**Response**
- **200 OK:** `ProductListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/user/products/v1"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { ProductListResponse } from "@/shared/models";

  const { data } = await clientFetch<ProductListResponse>("/api/user/products/v1");
  ```

