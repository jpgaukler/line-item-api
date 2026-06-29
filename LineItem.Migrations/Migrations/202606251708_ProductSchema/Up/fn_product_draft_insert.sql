CREATE OR REPLACE FUNCTION lineitem.product_draft_insert(
  p_product_data JSONB,
  p_base_product_id BIGINT,
  p_created_by BIGINT
)
  RETURNS SETOF lineitem.product_draft
AS
$$
BEGIN
  RETURN QUERY
    INSERT INTO lineitem.product_draft (base_product_id,
                                        base_version,
                                        product_data,
                                        created_by)
      VALUES (p_base_product_id,
              (SELECT active_version FROM lineitem.product WHERE id = p_base_product_id),
              p_product_data,
              p_created_by)
      RETURNING *;
END;
$$ LANGUAGE plpgsql;