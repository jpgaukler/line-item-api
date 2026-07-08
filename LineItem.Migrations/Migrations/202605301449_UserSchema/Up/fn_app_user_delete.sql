CREATE OR REPLACE FUNCTION lineitem.app_user_delete(p_id BIGINT)
  RETURNS VOID
AS
$$
BEGIN
  DELETE
  FROM lineitem.app_user
  WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;