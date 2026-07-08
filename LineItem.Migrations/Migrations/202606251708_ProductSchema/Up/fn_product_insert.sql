CREATE OR REPLACE FUNCTION lineitem.product_insert(
  p_product_category_id BIGINT,
  p_name VARCHAR,
  p_description VARCHAR,
  p_created_by BIGINT
)
  RETURNS SETOF lineitem.product
AS
$$
BEGIN
  RETURN QUERY
    INSERT INTO lineitem.product (product_category_id,
                                  name,
                                  description,
                                  created_by)
      VALUES (p_product_category_id,
              p_name,
              p_description,
              p_created_by)
      RETURNING *;
END;
$$ LANGUAGE plpgsql;