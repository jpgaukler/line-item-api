CREATE OR REPLACE FUNCTION lineitem.product_draft_update(
  p_id BIGINT,
  p_product_category_id BIGINT,
  p_product_data_json JSONB,
  p_updated_by BIGINT
)
  RETURNS VOID
AS
$$
BEGIN
  UPDATE lineitem.product_draft
  SET product_category_id = p_product_category_id,
      product_data_json   = p_product_data_json,
      updated_by          = p_updated_by
  WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;