CREATE OR REPLACE FUNCTION lineitem.product_category_delete(
  p_id BIGINT
)
  RETURNS VOID
AS
$$
BEGIN
  DELETE
  FROM lineitem.product_category
  WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;