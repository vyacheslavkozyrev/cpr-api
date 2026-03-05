-- Create feedback_request_recipients table
-- This table tracks individual recipient status within a feedback request

CREATE TABLE IF NOT EXISTS feedback_request_recipients (
    id uuid PRIMARY KEY,
    feedback_request_id uuid NOT NULL,
    employee_id uuid NOT NULL,
    is_completed boolean NOT NULL DEFAULT false,
    responded_at timestamptz,
    last_reminder_at timestamptz,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    
    -- Foreign keys
    CONSTRAINT FK_feedback_request_recipients_feedback_request_id 
        FOREIGN KEY (feedback_request_id) 
        REFERENCES feedback_requests(id) 
        ON DELETE CASCADE,
    
    CONSTRAINT FK_feedback_request_recipients_employee_id 
        FOREIGN KEY (employee_id) 
        REFERENCES employees(id) 
        ON DELETE CASCADE,
    
    -- Unique constraint: one recipient per request per employee
    CONSTRAINT UX_feedback_request_recipients_request_employee 
        UNIQUE (feedback_request_id, employee_id)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_feedback_request_id 
    ON feedback_request_recipients(feedback_request_id);

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_employee_id 
    ON feedback_request_recipients(employee_id);

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_is_completed 
    ON feedback_request_recipients(is_completed);

CREATE INDEX IF NOT EXISTS IX_feedback_request_recipients_pending 
    ON feedback_request_recipients(employee_id, is_completed) 
    WHERE is_completed = false;

-- Verify table was created
SELECT 'feedback_request_recipients table created successfully' AS status;
