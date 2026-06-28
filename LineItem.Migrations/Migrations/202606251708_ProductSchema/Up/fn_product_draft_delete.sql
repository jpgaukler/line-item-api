CREATE OR REPLACE FUNCTION lineitem.product_draft_delete(
  p_id BIGINT
)
  RETURNS VOID
AS
$$
BEGIN
  DELETE
  FROM lineitem.product_draft
  WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;