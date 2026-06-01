CREATE OR REPLACE FUNCTION lineitem.app_user_retrieve_by_id(
  p_id BIGINT
)
  RETURNS lineitem.app_user
AS
$$
DECLARE
  app_user lineitem.app_user;
BEGIN
  SELECT *
  INTO app_user
  FROM lineitem.app_user
  WHERE id = p_id;

  RETURN app_user;
END;
$$ LANGUAGE plpgsql;
