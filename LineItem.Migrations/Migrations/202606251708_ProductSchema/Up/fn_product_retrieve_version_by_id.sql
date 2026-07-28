CREATE OR REPLACE FUNCTION lineitem.product_retrieve_version_by_id(
  p_product_id BIGINT,
  p_version INT
)
  RETURNS SETOF lineitem.product_version_detail
AS
$$
BEGIN
  RETURN QUERY
    SELECT *
    FROM lineitem.product_version_detail
    WHERE id = p_product_id
      AND version = p_version;
END;
$$ LANGUAGE plpgsql STABLE;
