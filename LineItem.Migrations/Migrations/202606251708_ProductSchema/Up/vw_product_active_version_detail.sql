CREATE VIEW lineitem.product_active_version_detail AS
SELECT *
FROM lineitem.product_version_detail
WHERE version = active_version;