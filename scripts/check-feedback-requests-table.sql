-- Check the feedback_requests table structure
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'feedback_requests'
ORDER BY ordinal_position;
