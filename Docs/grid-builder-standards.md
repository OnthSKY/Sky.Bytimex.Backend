🧱 GridQueryBuilder Kullanım Dokümantasyonu / Usage Documentation

🇹🇷 Türkçe Açıklama

📌 Amaç

GridQueryBuilder, grid tabanlı sorgular için dinamik SQL oluşturur. Filtreleme, arama, sıralama ve sayfalama işlemlerini kolaylaştırmak için tasarlanmıştır.

⚙️ Parametreler

baseSql: Temel SELECT sorgusu. Örnek: SELECT * FROM expenses

GridRequest: Kullanıcının UI'den gönderdiği filtre, arama ve sayfalama verilerini içerir.

columnMappings: Filtre/sıralama için UI key'lerinin SQL kolonlarıyla eşleşmesi.

defaultOrderBy: Varsayılan sıralama kolonu.

likeFilterKeys: LIKE ile çalışacak filtre kolonları.

searchColumns: Arama yapılacak kolon listesi.

🔍 Özellikler

🔹 Arama

SearchValue alanı, searchColumns içinde tanımlı kolonlarda LIKE ile aranır.

🔹 Filtreleme

Filters sözlüğü üzerinden filtre uygulanır.

Eğer ilgili key likeFilterKeys içinde yer alıyorsa LIKE, değilse = ile filtrelenir.

🔹 Sıralama

OrderColumn ve OrderDirection değerlerine göre ORDER BY belirlenir.

Geçersiz durumda defaultOrderBy kullanılır.

🔹 Sayfalama

OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY

🔹 Sayı Sorgusu

SELECT COUNT(*) as count FROM ...

⚠️ Dikkat Edilmesi Gerekenler

baseSql içinde ORDER BY varsa, GenerateCountQuery bunu kırpar.

baseSql içinde mutlaka FROM ifadesi olmalıdır.

🇬🇧 English Description

📌 Purpose

GridQueryBuilder builds dynamic SQL queries for grid-based filtering, searching, sorting and paging.

⚙️ Parameters

baseSql: Base SELECT SQL. Example: SELECT * FROM expenses

GridRequest: Contains filter, search and paging data.

columnMappings: Maps frontend keys to actual SQL columns.

defaultOrderBy: Default sorting expression.

likeFilterKeys: Keys to be filtered with LIKE.

searchColumns: Columns for full-text search.

🔍 Features

🔹 Search

SearchValue is applied with LIKE on searchColumns.

🔹 Filtering

Filter logic uses Filters dictionary.

If key exists in likeFilterKeys → LIKE, otherwise → =.

🔹 Ordering

Uses OrderColumn and OrderDirection.

Falls back to defaultOrderBy if key is not mapped.

🔹 Paging

OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY

🔹 Count Query

SELECT COUNT(*) as count FROM ...

⚠️ Caveats

If ORDER BY exists, GenerateCountQuery trims it.

baseSql must contain a valid FROM clause.

Hazırlayan: Backend Utility Builder – SanShine.Cuzdan.Backend.Common.Utilities

