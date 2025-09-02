# Shared Module

### AuthController — POST /api/auth/sign-in
**Module:** Shared  
**Auth:** AllowAnonymous  
**Summary:** User login

**Request**
- **Route params:** none
- **Query params:** none
- **Body:** `AuthRequest`

**Response**
- **200 OK:** `AuthResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X POST "$BASE_URL/api/auth/sign-in" -H "Content-Type: application/json" -d '{}'
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { AuthRequest, AuthResponse } from "@/shared/models";

  const body: AuthRequest = {};
  const { data } = await clientFetch<AuthResponse>("/api/auth/sign-in", { method: "POST", body });
  ```

