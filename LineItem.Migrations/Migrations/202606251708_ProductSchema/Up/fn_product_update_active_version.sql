CREATE OR REPLACE FUNCTION lineitem.product_update_active_version(
  p_product_id BIGINT,
  p_active_version INT,
  p_updated_by BIGINT
)
  RETURNS VOID
AS
$$
BEGIN
  UPDATE lineitem.product
  SET active_version = p_active_version,
      updated_at     = NOW(),
      updated_by     = p_updated_by
  WHERE id = p_product_id;
END;
$$ LANGUAGE plpgsql;