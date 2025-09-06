# API Surface

### AuthController
Module: Shared

- **Endpoint:** `POST /api/auth/sign-in`
- **Frontend Usage:** 
- **Request:** Body `AuthRequest`
- **Response:** `AuthResponse`
- **Auth:** AllowAnonymous
- **Notes:** User login

### ProductController
Module: Admin

- **Endpoint:** `GET /api/admin/products/v1`
- **Frontend Usage:** 
- **Request:** Query `ProductFilterRequest`
- **Response:** `ProductListResponse`
- **Auth:** Authorize

### ProductCategoryController
Module: Admin

- **Endpoint:** `GET /api/admin/product-categories/v1`
- **Frontend Usage:** 
- **Request:** none
- **Response:** `ProductCategoryListResponse`
- **Auth:** Authorize

### BrandController
Module: Admin

- **Endpoint:** `GET /api/admin/brands/v2`
- **Frontend Usage:** 
- **Request:** Query `BrandFilterRequest`
- **Response:** `BrandListResponse`
- **Auth:** Authorize

### OrderController
Module: Admin

- **Endpoint:** `GET /api/admin/orders/v2`
- **Frontend Usage:** 
- **Request:** Query `OrderFilterRequest`
- **Response:** `OrderListResponse`
- **Auth:** Authorize

### PaymentController
Module: Admin

- **Endpoint:** `GET /api/admin/payments/v1`
- **Frontend Usage:** 
- **Request:** Query `PaymentFilterRequest`
- **Response:** `PaymentListResponse`
- **Auth:** Authorize

### ShipmentController
Module: Admin

- **Endpoint:** `GET /api/admin/shipments/v1`
- **Frontend Usage:** 
- **Request:** Query `ShipmentFilterRequest`
- **Response:** `ShipmentListResponse`
- **Auth:** Authorize

### RoleController
Module: Admin

- **Endpoint:** `GET /api/admin/roles/v1/all`
- **Frontend Usage:** 
- **Request:** Query `RoleFilterRequest`
- **Response:** `RoleListResponse`
- **Auth:** Authorize

### PermissionController
Module: Admin

- **Endpoint:** `GET /api/admin/permissions/v1/all`
- **Frontend Usage:** 
- **Request:** Query `PermissionFilterRequest`
- **Response:** `PermissionListResponse`
- **Auth:** Authorize

### ReturnController
Module: Admin

- **Endpoint:** `GET /api/admin/returns/v1`
- **Frontend Usage:** 
- **Request:** Query `ReturnFilterRequest`
- **Response:** `ReturnListResponse`
- **Auth:** Authorize

### InvoiceController
Module: Admin

- **Endpoint:** `GET /api/admin/invoices/v1`
- **Frontend Usage:** 
- **Request:** Query `InvoiceFilterRequest`
- **Response:** `InvoiceListResponse`
- **Auth:** Authorize

### KycController
Module: Admin

- **Endpoint:** `POST /api/admin/kyc/approve/v1`
- **Frontend Usage:** 
- **Request:** Body `KycApprovalRequest`
- **Response:** `KycStatusResponse`
- **Auth:** Authorize

### FileUploadController
Module: System

- **Endpoint:** `GET /api/file-uploads/v1`
- **Frontend Usage:** 
- **Request:** none
- **Response:** `FileUploadListResponse`
- **Auth:** Authorize

### ProductController (Vendor)
Module: Vendor

- **Endpoint:** `POST /api/vendor/products/v1`
- **Frontend Usage:** 
- **Request:** Body `CreateProductRequest`
- **Response:** `ProductDetailResponse`
- **Auth:** Authorize

### ProductController (User)
Module: Storefront

- **Endpoint:** `GET /api/user/products/v1`
- **Frontend Usage:** 
- **Request:** none
- **Response:** `ProductListResponse`
- **Auth:** AllowAnonymous

- **Auth:** AllowAnonymous

### VendorsController
Module: Storefront

- **Endpoint:** `GET /api/storefront/v1/vendors`
- **Frontend Usage:** Vendor listing
- **Request:** Query `GridRequest`
- **Response:** `VendorListPaginatedResponse`
- **Auth:** AllowAnonymous

- **Endpoint:** `GET /api/storefront/v1/vendors/detail`
- **Frontend Usage:** Vendor detail
- **Request:** Query `slug` or `id`
- **Response:** `VendorDetailResponse`
- **Auth:** AllowAnonymous

