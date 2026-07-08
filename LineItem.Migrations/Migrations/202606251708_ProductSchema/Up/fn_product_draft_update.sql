CREATE OR REPLACE FUNCTION lineitem.product_draft_update(
  p_id BIGINT,
  p_product_data JSONB,
  p_updated_by BIGINT
)
  RETURNS VOID
AS
$$
BEGIN
  UPDATE lineitem.product_draft
  SET product_data = p_product_data,
      updated_by   = p_updated_by
  WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;