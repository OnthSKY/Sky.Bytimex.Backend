# System Module

### FileUploadController — GET /api/file-uploads/v1
**Module:** System  
**Auth:** Authorize  
**Summary:** List file uploads

**Request**
- **Route params:** none
- **Query params:** none
- **Body:** `none`

**Response**
- **200 OK:** `FileUploadListResponse`
- **Errors:** 400/401/403/500

**Examples**
- **curl**
  ```bash
  curl -X GET "$BASE_URL/api/file-uploads/v1"
  ```
* **TypeScript**
  ```ts
  import { clientFetch } from "@/lib/api/http";
  import { FileUploadListResponse } from "@/shared/models";

  const { data } = await clientFetch<FileUploadListResponse>("/api/file-uploads/v1");
  ```

