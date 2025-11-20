-- Drop the employee_id column from feedback_requests table
-- This column is not needed since recipients are tracked in feedback_request_recipients

ALTER TABLE feedback_requests 
DROP COLUMN IF EXISTS employee_id;

-- Verify the column is gone
SELECT column_name 
FROM information_schema.columns 
WHERE table_name = 'feedback_requests' 
ORDER BY ordinal_position;
