import _ from 'lodash'
import {
    FETCH_BILLS,
} from '../actions/types'

export default (state = {}, action ) => {
    switch(action.type) {
        case FETCH_BILLS:
            return {...state, ..._.mapKeys(action.payload, 'id')};
        default:
            return state;
    }
};