CREATE OR REPLACE FUNCTION lineitem.product_version_insert(
  p_product_id BIGINT,
  p_product_data JSONB,
  p_created_by BIGINT
)
  RETURNS SETOF lineitem.product_version
AS
$$
DECLARE
  v_next_version INT;
BEGIN
  SELECT COALESCE(MAX(version), 0) + 1
  INTO v_next_version
  FROM lineitem.product_version
  WHERE product_id = p_product_id;

  RETURN QUERY
    INSERT INTO lineitem.product_version (product_id,
                                          version,
                                          product_data,
                                          created_by)
      VALUES (p_product_id,
              v_next_version,
              p_product_data,
              p_created_by)
      RETURNING *;
END;
$$ LANGUAGE plpgsql;