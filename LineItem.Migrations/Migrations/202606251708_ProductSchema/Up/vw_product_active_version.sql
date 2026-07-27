CREATE VIEW lineitem.product_active_version AS
SELECT p.id,
       p.product_category_id,
       p.name,
       p.description,
       pv.version,
       pv.product_data,
       pv.created_at,
       pv.created_by
FROM lineitem.product p
       INNER JOIN lineitem.product_version pv
                  ON pv.product_id = p.id
                    AND pv.version = p.active_version;