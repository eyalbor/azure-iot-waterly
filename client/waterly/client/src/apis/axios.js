import axios from 'axios'

export default axios.create({
    baseURL: 'http://localhost:7071/api'
    //baseURL: 'https://waterly-iot-functions.azurewebsites.net/api'
})