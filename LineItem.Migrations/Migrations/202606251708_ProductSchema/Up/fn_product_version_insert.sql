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
  v_product_data JSONB;
BEGIN
  SELECT COALESCE(MAX(version), 0) + 1
  INTO v_next_version
  FROM lineitem.product_version
  WHERE product_id = p_product_id;

  -- Add the product id and version to the product data before inserting
  v_product_data := p_product_data || JSONB_BUILD_OBJECT(
      'id', p_product_id,
      'version', v_next_version);

  RETURN QUERY
    INSERT INTO lineitem.product_version (product_id,
                                          version,
                                          product_data,
                                          created_by)
      VALUES (p_product_id,
              v_next_version,
              v_product_data,
              p_created_by)
      RETURNING *;
END;
$$ LANGUAGE plpgsql;