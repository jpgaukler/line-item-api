CREATE OR REPLACE FUNCTION lineitem.product_delete(
  p_id BIGINT
)
  RETURNS VOID
AS
$$
BEGIN
  -- Remove the active_version FK constraint temporarily by nulling it out
  UPDATE lineitem.product
  SET active_version = NULL
  WHERE id = p_id;

  -- Delete all version rows
  DELETE
  FROM lineitem.product_version
  WHERE product_id = p_id;

  -- Delete the product row
  DELETE
  FROM lineitem.product
  WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;