import {
    SIGN_IN,
    SIGN_OUT,
    SET_USER,

    CREATE_DEVICE,
    FETCH_DEVICE,
    FETCH_DEVICES,
    DELETE_DEVICE,
    EDIT_DEVICE,
    QUALITY_DEVICE,

    FETCH_EVENTS,

    FETCH_NOTIFICATIONS,
    EDIT_NOTIFICATION,

    FETCH_BILLS,
    PAY_BILL
} from './types'
import myUrl from '../apis/axios'
import history from '../history'

export const signIn = (userId) =>{
    return {
        type: SIGN_IN,
        payload: userId
    }
}; 

export const signOut = () => {
    return {
        type: SIGN_OUT
    }
};

export const setUser = (user) =>  (dispatch) => {
    //console.log(user)
    dispatch({type: SET_USER, payload: user})
}

export const consumptionForYearEachMonth = (userId, year) => {
    //return myUrl.get(`/consumption_per_month/userId=${userId}`)
    return myUrl.get(`/consumption_per_month/userId=${userId}&year=${year}`)
}

export const fetchNotifications = () => async (dispatch, getState) => {
    const { userId } = getState().auth;
    const response = await myUrl.get(`/notifications/user_id=${userId}`);
    //console.log('fetchNotifications')
    //console.log(response.data)
    dispatch({type: FETCH_NOTIFICATIONS, payload: response.data})
};

export const updateNotification = (notification) => async (dispatch) => {
    //console.log(notification)
    const response =  await myUrl.patch(`/notifications/${notification.id}`, notification)
    //console.log('updateNotification')
    //console.log(response.data)
    dispatch({type: EDIT_NOTIFICATION, payload: response.data})
};

export const createDevice = (userId,formValues) => async (dispatch, getState) => {
    const { userId } = getState().auth;
    const response = await myUrl.post(`/devices/${userId}`, {...formValues, userId });
    dispatch({type: CREATE_DEVICE, payload: response.data})
    history.push('devices/list');
};

export const fetchDevices = () =>  async (dispatch, getState) => {
    const { userId } = getState().auth;
    //return myUrl.get(`/devices/${userId}`);
    const response = await myUrl.get(`/devices/userId=${userId}`);
    //console.log('fetchDevices')
    //console.log(response.data)
    dispatch({type: FETCH_DEVICES, payload: response.data})
};

export const fetchDevice = (id) => async dispatch => {
    const response = await myUrl.get(`/devices/${id}`);
    //console.log('fetchDevice')
    //console.log(response.data)
    dispatch({type: FETCH_DEVICE, payload: response.data})
}

export const editDevice = (id,formValues) => async dispatch => {
    const response = await myUrl.patch(`/devices/${id}`, formValues);
    dispatch({type: EDIT_DEVICE, payload: response.data})
    history.push('/devices/list');
}

export const deleteDevice = (id) => async dispatch => {
    await myUrl.delete(`/devices/${id}`);
    dispatch({type: DELETE_DEVICE, payload: id})
}

export const fetchEvents = (deviceId) => async (dispatch) => {
    const response = await myUrl.get(`/events/device_id=${deviceId}`);
    //console.log('fetchEvents')
    //console.log(response.data)
    dispatch({type: FETCH_EVENTS, payload: response.data})
};

export const fetchBills = () => async (dispatch, getState) => {
    const { userId } = getState().auth;
    const response = await myUrl.get(`/bills/userId=${userId}`);
    //console.log('fetchBills')
    //console.log(response.data)
    dispatch({type: FETCH_BILLS, payload: response.data})
}

export const payForBill = (bill) => async (dispatch) => {
    //console.log('payForBill')   
    const response = await myUrl.patch(`/bills/${bill.id}`, bill);
    //console.log('payForBill')
    //console.log(response)
    dispatch({type: PAY_BILL, payload: response.data})
}

export const quality = (device_id) => async (dispatch) => {
    //console.log('quality')   
    const response = await myUrl.get(`/quality/device_id=${device_id}`);
    //console.log(response.data)
    dispatch({type: QUALITY_DEVICE, payload: response.data})
}