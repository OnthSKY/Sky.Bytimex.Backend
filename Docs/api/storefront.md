# Storefront Module

### ProductController — GET /api/user/products/v1
**Module:** Storefront  
**Auth:** AllowAnonymous  
**Summary:** List products for storefront

**Request**
- **Route params:** none
- **Query params:** supports `filters[vendor_slug]`
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

### VendorsController — GET /api/storefront/v1/vendors
**Module:** Storefront
**Auth:** AllowAnonymous
**Summary:** List public vendors

**Request**
- **Query params:** supports pagination, search, and `filters[slug]`

**Response**
- **200 OK:** `VendorListPaginatedResponse`

### VendorsController — GET /api/storefront/v1/vendors/detail
**Module:** Storefront
**Auth:** AllowAnonymous
**Summary:** Get vendor by slug or id

**Request**
- **Query params:** `slug` or `id`

**Response**
- **200 OK:** `VendorDetailResponse`
- **404:** Vendor not found

