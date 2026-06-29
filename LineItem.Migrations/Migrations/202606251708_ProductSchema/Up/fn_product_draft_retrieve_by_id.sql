CREATE OR REPLACE FUNCTION lineitem.product_draft_retrieve_by_id(
  p_id BIGINT
)
  RETURNS SETOF lineitem.product_draft
AS
$$
BEGIN
  RETURN QUERY
    SELECT *
    FROM lineitem.product_draft
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql STABLE;