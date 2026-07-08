CREATE OR REPLACE FUNCTION lineitem.product_category_update(
  p_id BIGINT,
  p_name VARCHAR,
  p_updated_by BIGINT
)
  RETURNS SETOF lineitem.product_category
AS
$$
BEGIN
  RETURN QUERY
    UPDATE lineitem.product_category
      SET name = p_name,
        updated_by = p_updated_by
      WHERE id = p_id
      RETURNING *;
END;
$$ LANGUAGE plpgsql;