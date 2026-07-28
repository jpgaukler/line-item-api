CREATE OR REPLACE FUNCTION lineitem.product_retrieve_by_category_id(
  p_category_id BIGINT
)
  RETURNS SETOF lineitem.product_version_detail
AS
$$
BEGIN
  RETURN QUERY
    SELECT * FROM lineitem.product_active_version_detail WHERE product_category_id = p_category_id;
END;
$$ LANGUAGE plpgsql STABLE;