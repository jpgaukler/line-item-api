CREATE VIEW lineitem.product_version_detail AS
SELECT p.id,
       p.product_category_id,
       p.name,
       p.description,
       p.active_version,
       pv.version,
       pv.product_data_json,
       pv.created_at,
       pv.created_by
FROM lineitem.product p
       INNER JOIN lineitem.product_version pv
                  ON pv.product_id = p.id;