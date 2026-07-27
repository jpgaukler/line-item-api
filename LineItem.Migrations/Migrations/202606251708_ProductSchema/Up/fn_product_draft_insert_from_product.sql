CREATE OR REPLACE FUNCTION lineitem.product_draft_insert_from_product(
  p_base_product_id BIGINT,
  p_created_by BIGINT
)
  RETURNS SETOF lineitem.product_draft
AS
$$
BEGIN
  RETURN QUERY
    INSERT INTO lineitem.product_draft (product_category_id,
                                        base_product_id,
                                        base_version,
                                        product_data,
                                        created_by)
      SELECT p.product_category_id,
             p.id,
             p.active_version,
             pv.product_data,
             p_created_by
      FROM lineitem.product p
             INNER JOIN lineitem.product_version pv
                        ON pv.product_id = p.id
                          AND pv.version = p.active_version
      WHERE p.id = p_base_product_id
      RETURNING *;
END;
$$ LANGUAGE plpgsql;