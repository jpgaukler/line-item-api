CREATE OR REPLACE FUNCTION lineitem.product_retrieve_active_version_by_id(
  p_id BIGINT
)
  RETURNS SETOF lineitem.product_version_detail
AS
$$
BEGIN
  RETURN QUERY
    SELECT * FROM lineitem.product_active_version_detail WHERE id = p_id;
END;
$$ LANGUAGE plpgsql STABLE;