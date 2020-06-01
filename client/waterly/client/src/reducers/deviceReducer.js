import _ from 'lodash'
import {
    FETCH_DEVICE,
    FETCH_DEVICES,
    CREATE_DEVICE,
    EDIT_DEVICE,
    DELETE_DEVICE
} from '../actions/types'

export default (state = {}, action ) => {
    switch(action.type) {
        case FETCH_DEVICES:
            return {...state, ..._.mapKeys(action.payload, 'device_id')};
        case FETCH_DEVICE:
            return {...state, [action.payload.id]: action.payload};
        case CREATE_DEVICE:
            return {...state, [action.payload.id]: action.payload};
        case EDIT_DEVICE:
            return {...state, [action.payload.id]: action.payload};
        case DELETE_DEVICE:
            return _.omit(state, action.payload);
        default:
            return state;
    }
};