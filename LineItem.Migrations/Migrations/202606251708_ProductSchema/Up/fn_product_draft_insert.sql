CREATE OR REPLACE FUNCTION lineitem.product_draft_insert(
  p_product_category_id BIGINT,
  p_product_data_json JSONB,
  p_created_by BIGINT
)
  RETURNS SETOF lineitem.product_draft
AS
$$
BEGIN
  RETURN QUERY
    INSERT INTO lineitem.product_draft (product_category_id,
                                        product_data_json,
                                        created_by)
      VALUES (p_product_category_id,
              p_product_data_json,
              p_created_by)
      RETURNING *;
END;
$$ LANGUAGE plpgsql;