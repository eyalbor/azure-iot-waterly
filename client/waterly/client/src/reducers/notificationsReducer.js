import _ from 'lodash'
import {
    FETCH_NOTIFICATIONS
} from '../actions/types'

export default (state = {}, action ) => {
    switch(action.type) {
        case FETCH_NOTIFICATIONS:
            return {...state, ..._.mapKeys(action.payload, 'id')};
        default:
            return state;
    }
};