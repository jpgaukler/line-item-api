CREATE OR REPLACE FUNCTION lineitem.product_retrieve_active_version_by_id(
  p_id BIGINT
)
  RETURNS SETOF lineitem.product_version
AS
$$
BEGIN
  RETURN QUERY
    SELECT pv.*
    FROM lineitem.product_version pv
           INNER JOIN lineitem.product p
                      ON p.id = pv.product_id
                        AND p.active_version = pv.version
    WHERE p.id = p_id;
END;
$$ LANGUAGE plpgsql STABLE;