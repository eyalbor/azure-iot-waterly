# Requires pip install requests && pip freeze > requirements.txt
import requests

payload = {
    "email": "eyalbor@gmail.com",
    "invoice": "23",
    "amount": "100",
    "task": "!"
}
response = requests.post("https://prod-26.eastus.logic.azure.com:443/workflows/06a66aa325a84a29b64f788ff1537d50/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-f_3sTNheCjl7dSq3dZzCuqkYChEXDcweiK92DVv_KU")
print(response.status_code)