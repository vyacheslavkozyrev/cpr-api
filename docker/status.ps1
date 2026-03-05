#!/usr/bin/env pwsh
# Quick start script for CPR Docker services

Write-Host "🐳 CPR Docker Services" -ForegroundColor Cyan
Write-Host ""

$services = docker ps --filter "name=cpr_" --format "{{.Names}}"

if ($services) {
    Write-Host "✅ Running services:" -ForegroundColor Green
    docker ps --filter "name=cpr_" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
} else {
    Write-Host "⚠️  No CPR services are running" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Available commands:" -ForegroundColor Cyan
Write-Host "  docker-compose up -d              # Start all services"
Write-Host "  docker-compose up -d db_dev       # Start dev database"
Write-Host "  docker-compose up -d smtp4dev     # Start email server"
Write-Host "  docker-compose down               # Stop all services"
Write-Host "  docker-compose logs -f            # View logs"
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor Cyan
Write-Host "  PostgreSQL (dev):  localhost:5432"
Write-Host "  PostgreSQL (test): localhost:5433"
Write-Host "  smtp4dev Web UI:   http://localhost:3333"
Write-Host ""
