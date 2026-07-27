CREATE OR REPLACE FUNCTION lineitem.product_search(
  p_search_term VARCHAR
)
  RETURNS SETOF lineitem.product_active_version
AS
$$
BEGIN
  RETURN QUERY
    SELECT *
    FROM lineitem.product_active_version pav
    WHERE WORD_SIMILARITY(p_search_term, pav.name) > 0.3
       OR WORD_SIMILARITY(p_search_term, pav.description) > 0.3
    ORDER BY GREATEST(
                 WORD_SIMILARITY(p_search_term, pav.name),
                 WORD_SIMILARITY(p_search_term, pav.description)
             ) DESC;
END;
$$ LANGUAGE plpgsql STABLE;


-- TODO: DETERMINE IF THE INDEX IS GETTING PICKED UP ABOVE... MIGHT NEED TO TRY THIS ALTERNATE QUERY
-- CREATE OR REPLACE FUNCTION lineitem.product_search(
--   p_search_term VARCHAR
-- )
--   RETURNS SETOF lineitem.product_active_version
-- AS
-- $$
-- BEGIN
--   PERFORM SET_LIMIT(0.3); -- sets pg_trgm.word_similarity_threshold for this session/call
-- 
--   RETURN QUERY
--     SELECT *
--     FROM lineitem.product_active_version pav
--     WHERE pav.name % p_search_term
--        OR pav.description % p_search_term
--     ORDER BY GREATEST(
--                  WORD_SIMILARITY(p_search_term, pav.name),
--                  WORD_SIMILARITY(p_search_term, pav.description)
--              ) DESC;
-- END;
-- $$ LANGUAGE plpgsql STABLE;