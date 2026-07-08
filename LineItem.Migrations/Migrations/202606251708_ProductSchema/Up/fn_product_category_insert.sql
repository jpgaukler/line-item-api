CREATE OR REPLACE FUNCTION lineitem.product_category_insert(
  p_name VARCHAR,
  p_created_by BIGINT
)
  RETURNS SETOF lineitem.product_category
AS
$$
BEGIN
  RETURN QUERY
    INSERT INTO lineitem.product_category (name, created_by)
      VALUES (p_name, p_created_by)
      RETURNING *;
END;
$$ LANGUAGE plpgsql;