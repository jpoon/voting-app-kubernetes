docker build -t vote .
docker run --rm -e AZURE_STORAGE_ACCOUNT=japoonvotingapp -e AZURE_STORAGE_ACCESS_KEY="V77H7l9bRm6zn+WCQPunuasO6eeb4lLLj5D9AMsZqSFsectA8Bb2Qk/B6Ch57SLF9oGvEq4zYDLd3sF7PhRE1A==" -p 8080:80 vote
