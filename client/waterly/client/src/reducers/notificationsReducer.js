import _ from 'lodash'
import {
    FETCH_NOTIFICATIONS,
    EDIT_NOTIFICATION
} from '../actions/types'

export default (state = {}, action ) => {
    switch(action.type) {
        case FETCH_NOTIFICATIONS:
            return {...state,  ..._.mapKeys(action.payload, 'id')};
        case EDIT_NOTIFICATION:
            return {...state, [action.payload.id]: action.payload};
        default:
            return state;
    }
};