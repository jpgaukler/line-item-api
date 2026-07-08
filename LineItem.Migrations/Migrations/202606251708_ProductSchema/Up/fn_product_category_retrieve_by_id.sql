CREATE OR REPLACE FUNCTION lineitem.product_category_retrieve_by_id(
  p_id BIGINT
)
  RETURNS SETOF lineitem.product_category
AS
$$
BEGIN
  RETURN QUERY
    SELECT *
    FROM lineitem.product_category
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql STABLE;