import {
    SIGN_IN,
    SIGN_OUT,
    CREATE_DEVICE,
    FETCH_DEVICE,
    FETCH_DEVICES,
    DELETE_DEVICE,
    EDIT_DEVICE,
    FETCH_EVENTS
} from './types'
import myUrl from '../apis/axios'

import history from '../history'

export const signIn = (userId) =>{
    return {
        type: SIGN_IN,
        payload: userId
    }
}; 

export const signOut = ()=>{
    return {
        type: SIGN_OUT
    }
}; 

export const createDevice = (formValues) => async (dispatch, getState) => {
    const { userId } = getState().auth;
    const response = await myUrl.post('/devices', {...formValues, userId });
    dispatch({type: CREATE_DEVICE, payload: response.data})
    //programmatic navigation to streamList
    history.push('devices/list');
};

export const fetchDevices = () => async (dispatch) => {
    const response = await myUrl.get('/devices');
    dispatch({type: FETCH_DEVICES, payload: response.data})
};

export const fetchDevice = (id) => async dispatch => {
    const response = await myUrl.get(`/devices/${id}`);
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

export const fetchEvents = () => async (dispatch) => {
    const response = await myUrl.get('/events');
    dispatch({type: FETCH_EVENTS, payload: response.data})
};